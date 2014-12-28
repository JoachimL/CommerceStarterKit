using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Inventory;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;

namespace OxxCommerceStarterKit.Web.Business.Payment
{
    public class StockUpdater : IStockUpdater
    {
        private static readonly ILogger Log = LogManager.GetLogger();
            
        public void AdjustStocks(PurchaseOrderModel order)
        {
            var warehouseRepository = ServiceLocator.Current.GetInstance<IWarehouseRepository>();
            var warehousesCache = warehouseRepository.List();
            var warehouseInventory = ServiceLocator.Current.GetInstance<IWarehouseInventoryService>();

            var expirationCandidates = new HashSet<ProductContent>();

            var referenceConverter = ServiceLocator.Current.GetInstance<ReferenceConverter>();
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

            // Adjust inventory
            foreach (var f in order.OrderForms)
            {
                foreach (var i in f.LineItems)
                {
                    try
                    {
                        var warehouse = warehousesCache.First(w => w.Code == i.WarehouseCode);
                        var catalogEntry = CatalogContext.Current.GetCatalogEntry(i.CatalogEntryId);
                        var catalogKey = new CatalogKey(catalogEntry);
                        var inventory = new WarehouseInventory(warehouseInventory.Get(catalogKey, warehouse));

                        if ((inventory.InStockQuantity -= i.Quantity) <= 0)
                        {
                            var contentLink = referenceConverter.GetContentLink(i.CatalogEntryId);
                            var variant = contentRepository.Get<VariationContent>(contentLink);

                            expirationCandidates.Add((ProductContent)variant.GetParent());
                        }

                        warehouseInventory.Save(inventory);
                    }
                    catch (Exception ex)
                    {
                        LoggerExtensions.Error((ILogger) Log, "Unable to adjust inventory.", ex);
                    }

                }
            }

            // TODO: Determine if you want to unpublish products with no sellable variants
            // ExpireProductsWithNoInventory(expirationCandidates, contentRepository);
            // Alterntive approach is to notify the commerce admin about the products without inventory

        }
    }
}