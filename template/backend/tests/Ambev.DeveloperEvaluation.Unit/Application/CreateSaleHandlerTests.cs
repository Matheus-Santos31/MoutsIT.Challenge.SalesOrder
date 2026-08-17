using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CreateSaleHandler"/> class — completing an
/// active cart into a sale, including the address gate and the snapshot/denormalization
/// rules for customer, branch and product data.
/// </summary>
public class CreateSaleHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IBranchAddressRepository _branchAddressRepository;
    private readonly IUserAddressRepository _userAddressRepository;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _saleRepository = Substitute.For<ISaleRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _branchRepository = Substitute.For<IBranchRepository>();
        _branchAddressRepository = Substitute.For<IBranchAddressRepository>();
        _userAddressRepository = Substitute.For<IUserAddressRepository>();

        _handler = new CreateSaleHandler(
            _cartRepository, _saleRepository, _userRepository,
            _branchRepository, _branchAddressRepository, _userAddressRepository);
    }

    private sealed record Scenario(Cart Cart, Branch Branch, User User, BranchAddress BranchAddress, UserAddress UserAddress);

    private Scenario BuildValidScenario(int quantity = 5)
    {
        var userId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), Title = "Beer", Description = "Cold one", Category = ProductCategory.Food, Price = 10m };
        var cartItem = new CartItem { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Quantity = quantity };
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId, BranchId = branchId, Status = CartStatus.Active, Items = [cartItem] };

        var branch = new Branch { Id = branchId, Name = "Downtown", DocNumber = "123456", CompanyName = "Acme Ltda" };
        var user = new User { Id = userId, FirstName = "Jane", LastName = "Doe", Email = "jane@doe.com" };

        var address = new Address { Id = Guid.NewGuid(), City = "SP", Street = "Rua A", Number = 1, PostalCode = "00000-000" };
        var branchAddress = new BranchAddress { BranchId = branchId, Address = address };
        var userAddress = new UserAddress { UserId = userId, Address = address, IsDefault = true, IsActive = true };

        _cartRepository.GetByIdWithItemsAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);
        _saleRepository.GetByCartIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns((Sale?)null);
        _branchAddressRepository.GetByBranchIdAsync(branchId, Arg.Any<CancellationToken>()).Returns(branchAddress);
        _userAddressRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(new List<UserAddress> { userAddress });
        _branchRepository.GetByIdAsync(branchId, Arg.Any<CancellationToken>()).Returns(branch);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        return new Scenario(cart, branch, user, branchAddress, userAddress);
    }

    [Fact(DisplayName = "Given a valid active cart When creating a sale Then snapshots data and completes the cart")]
    public async Task Handle_ValidActiveCart_CreatesSaleAndCompletesCart()
    {
        var scenario = BuildValidScenario(quantity: 5);
        var command = new CreateSaleCommand { CartId = scenario.Cart.Id, RequestingUserId = scenario.Cart.UserId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(SaleStatus.Created);
        result.ProductsQuantity.Should().Be(1);
        result.ItemsQuantity.Should().Be(5);
        result.TotalDiscount.Should().Be(5m); // 5 * 10 * 10%
        result.TotalAmount.Should().Be(45m); // 50 - 5
        result.CustomerName.Should().Be("Jane Doe");
        result.BranchDocNumber.Should().Be("123456");
        scenario.Cart.Status.Should().Be(CartStatus.Completed);
        await _saleRepository.Received(1).AddAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a cart that is not active When creating a sale Then throws DomainException")]
    public async Task Handle_CartNotActive_ThrowsDomainException()
    {
        var scenario = BuildValidScenario();
        scenario.Cart.Status = CartStatus.Cancelled;
        var command = new CreateSaleCommand { CartId = scenario.Cart.Id, RequestingUserId = scenario.Cart.UserId };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact(DisplayName = "Given an empty cart When creating a sale Then throws DomainException")]
    public async Task Handle_EmptyCart_ThrowsDomainException()
    {
        var scenario = BuildValidScenario();
        scenario.Cart.Items.Clear();
        var command = new CreateSaleCommand { CartId = scenario.Cart.Id, RequestingUserId = scenario.Cart.UserId };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact(DisplayName = "Given a cart already completed into a sale When creating again Then throws DomainException")]
    public async Task Handle_CartAlreadyHasSale_ThrowsDomainException()
    {
        var scenario = BuildValidScenario();
        _saleRepository.GetByCartIdAsync(scenario.Cart.Id, Arg.Any<CancellationToken>())
            .Returns(new Sale { CartId = scenario.Cart.Id });
        var command = new CreateSaleCommand { CartId = scenario.Cart.Id, RequestingUserId = scenario.Cart.UserId };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact(DisplayName = "Given a branch without a registered address When creating a sale Then throws DomainException")]
    public async Task Handle_BranchWithoutAddress_ThrowsDomainException()
    {
        var scenario = BuildValidScenario();
        _branchAddressRepository.GetByBranchIdAsync(scenario.Cart.BranchId, Arg.Any<CancellationToken>())
            .Returns((BranchAddress?)null);
        var command = new CreateSaleCommand { CartId = scenario.Cart.Id, RequestingUserId = scenario.Cart.UserId };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact(DisplayName = "Given a user without a registered address When creating a sale Then throws DomainException")]
    public async Task Handle_UserWithoutAddress_ThrowsDomainException()
    {
        var scenario = BuildValidScenario();
        _userAddressRepository.GetByUserIdAsync(scenario.Cart.UserId, Arg.Any<CancellationToken>())
            .Returns(new List<UserAddress>());
        var command = new CreateSaleCommand { CartId = scenario.Cart.Id, RequestingUserId = scenario.Cart.UserId };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact(DisplayName = "Given a user who does not own the cart When creating a sale Then throws UnauthorizedAccessException")]
    public async Task Handle_NonOwnerNonAdmin_ThrowsUnauthorizedAccessException()
    {
        var scenario = BuildValidScenario();
        var command = new CreateSaleCommand { CartId = scenario.Cart.Id, RequestingUserId = Guid.NewGuid(), IsRequestingUserAdmin = false };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
