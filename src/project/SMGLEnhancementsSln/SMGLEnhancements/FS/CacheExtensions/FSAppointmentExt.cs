using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.FS;
using PX.Objects.GL;

namespace SMGLEnhancements.FS
{
    public sealed class FSAppointmentExt : PXCacheExtension<FSAppointment>
    {
        public static bool IsActive() => true;

        #region UsrBatchNbr
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Batch Nbr", Enabled = false)]
        [PXSelector(typeof(
            SelectFrom<Batch>
                .Where<Batch.module.IsEqual<BatchModule.moduleGL>>
            .SearchFor<Batch.batchNbr>))]
        public string UsrBatchNbr { get; set; }
        public abstract class usrBatchNbr : BqlString.Field<usrBatchNbr> { }
        #endregion
    }
}
