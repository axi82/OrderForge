using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using OrderForge.Client.Models;
using OrderForge.Client.Services;

namespace OrderForge.Client.Pages.Supplier.Admin.Products;

public partial class Index
{
    private const int DefaultPageSize = 25;

    private AdminProductsListResult? result;
    private string searchText = string.Empty;
    private int page = 1;
    private int pageSize = DefaultPageSize;
    private bool loading = true;
    private string? errorMessage;
    private string? createErrorMessage;
    private bool saving;
    private int? deletingId;
    private readonly CreateProductRequest createModel = new();

    private ElementReference _addProductDialog;
    private AdminProductSearchBar? _searchBar;
    private bool isSupplierAdmin;
    private bool focusSkuAfterRender;
    private int addProductFormKey;

    private int MaxPage => result is null || result.TotalCount == 0
        ? 1
        : Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)pageSize));

    private int ShowingStart => result is null || result.TotalCount == 0 ? 0 : (page - 1) * pageSize + 1;

    private int ShowingEnd => result is null ? 0 : Math.Min(page * pageSize, result.TotalCount);

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        isSupplierAdmin = (await AuthorizationService.AuthorizeAsync(authState.User, AuthorizationPolicies.SupplierAdmin))
            .Succeeded;
        await LoadAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusSkuAfterRender)
        {
            focusSkuAfterRender = false;
            await Js.InvokeVoidAsync("orderForgeDialog.focusById", "admin-product-modal-sku");
        }
    }

    private async Task ApplySearchAsync()
    {
        page = 1;
        await LoadAsync();
    }

    private async Task GoToPrevPageAsync()
    {
        if (page <= 1)
        {
            return;
        }

        page--;
        await LoadAsync();
    }

    private async Task GoToNextPageAsync()
    {
        if (page >= MaxPage)
        {
            return;
        }

        page++;
        await LoadAsync();
    }

    private async Task OnPageSizeChanged(ChangeEventArgs e)
    {
        if (e.Value is string s && int.TryParse(s, out var n) && n is 10 or 25 or 50)
        {
            pageSize = n;
            page = 1;
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        loading = true;
        errorMessage = null;
        try
        {
            var search = string.IsNullOrWhiteSpace(searchText) ? null : searchText.Trim();
            result = await AdminApi.GetProductsAsync(page, pageSize, search);
            if (page > MaxPage && MaxPage >= 1)
            {
                page = MaxPage;
                result = await AdminApi.GetProductsAsync(page, pageSize, search);
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            result = null;
        }
        finally
        {
            loading = false;
        }
    }

    private void ResetCreateModel()
    {
        createModel.Sku = string.Empty;
        createModel.ProductCode = string.Empty;
        createModel.Name = string.Empty;
        createModel.ShortDescription = string.Empty;
        createModel.Description = null;
        createModel.Brand = null;
        createModel.CommodityCodeDescription = null;
        createModel.SupplierAccountCode = null;
        createModel.PartNumber = null;
        createModel.QuantityInStock = 0;
        createModel.QuantityAllocated = 0;
        createModel.QuantityOnOrder = 0;
        createModel.FreeStock = 0;
        createModel.Barcode = null;
        createModel.CostPrice = 0;
        createModel.BasePrice = 0;
        createModel.IsActive = true;
    }

    private async Task OpenAddProductDialogAsync()
    {
        addProductFormKey++;
        ResetCreateModel();
        createErrorMessage = null;
        await Js.InvokeVoidAsync("orderForgeDialog.showModal", _addProductDialog);
        focusSkuAfterRender = true;
        await InvokeAsync(StateHasChanged);
    }

    private async Task CloseAddProductDialogUiAsync()
    {
        await Js.InvokeVoidAsync("orderForgeDialog.close", _addProductDialog);
        if (_searchBar is not null)
        {
            await _searchBar.FocusAddProductButtonAsync();
        }
    }

    private async Task DismissAddProductDialogAsync()
    {
        ResetCreateModel();
        createErrorMessage = null;
        await CloseAddProductDialogUiAsync();
    }

    private async Task OnAddProductDialogCancelAsync()
    {
        ResetCreateModel();
        createErrorMessage = null;
        if (_searchBar is not null)
        {
            await _searchBar.FocusAddProductButtonAsync();
        }
    }

    private Task OnAddProductDialogBackdropAsync(MouseEventArgs _) => DismissAddProductDialogAsync();

    private async Task OnCreateSubmitAsync(AdminProductSubmitPayload payload)
    {
        createErrorMessage = null;
        saving = true;
        try
        {
            await AdminApi.CreateProductWithImagesAsync(
                payload.Model,
                payload.ImageFiles,
                payload.MainImageIndex);
            ResetCreateModel();
            page = 1;
            await LoadAsync();
            await CloseAddProductDialogUiAsync();
        }
        catch (Exception ex)
        {
            createErrorMessage = ex.Message;
        }
        finally
        {
            saving = false;
        }
    }

    private async Task DeleteAsync(ProductDto row)
    {
        if (!await Js.InvokeAsync<bool>("confirm", new object[] { $"Delete product \"{row.Name}\" (SKU {row.Sku})?" }))
        {
            return;
        }

        deletingId = row.Id;
        errorMessage = null;
        try
        {
            await AdminApi.DeleteProductAsync(row.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            deletingId = null;
        }
    }
}
