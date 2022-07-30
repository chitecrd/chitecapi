using DataAccess.Models;
using Infrastructure.Repositories;
using System.Data;
using System.Data.SqlClient;

namespace Infrastructure.UnitOfWork
{
    public class PurchaseOrderUnitOfWork : GenericTransactionalUnitOfWork
    {
        public PurchaseOrderUnitOfWork(string connectionString) : base(new SqlConnection(connectionString))
        { }


        public PurchaseOrderUnitOfWork(string connectionString, IDbConnection conection)
          : base(conection)
        { }

        private IGenericRepository<PurchaseOrderHeader> purchaseOrderHeaderRepository;
        public IGenericRepository<PurchaseOrderHeader> PurchaseOrderHeaderRepository =>
            purchaseOrderHeaderRepository ?? (purchaseOrderHeaderRepository = new PurchaseOrderHeaderRepository(dataBaseAccess));

        private IGenericRepository<PurchaseOrderDetail> purchaseOrderDetailRepository;
        public IGenericRepository<PurchaseOrderDetail> PurchaseOrderDetailRepository =>
            purchaseOrderDetailRepository ?? (purchaseOrderDetailRepository = new PurchaseOrderDetailRepository(dataBaseAccess));

        private IGenericRepository<EncabezadoRecepcionOrdenCompra> encabezadoRecepcionOrdenCompraRepository;
        public IGenericRepository<EncabezadoRecepcionOrdenCompra> EncabezadoRecepcionOrdenCompraRepository =>
            encabezadoRecepcionOrdenCompraRepository ?? (encabezadoRecepcionOrdenCompraRepository = new EncabezadoRecepcionOrdenCompraRepository(dataBaseAccess));

        private IGenericRepository<DetalleRecepcionOrdenCompra> detalleRecepcionOrdenCompraRepository;
        public IGenericRepository<DetalleRecepcionOrdenCompra> DetalleRecepcionOrdenCompraRepository =>
            detalleRecepcionOrdenCompraRepository ?? (detalleRecepcionOrdenCompraRepository = new DetalleRecepcionOrdenCompraRepository(dataBaseAccess));

        protected override void ResetRepositories()
        {
            purchaseOrderHeaderRepository = null;
            purchaseOrderDetailRepository = null;
            encabezadoRecepcionOrdenCompraRepository = null;
            detalleRecepcionOrdenCompraRepository = null;
        }
    }
}
