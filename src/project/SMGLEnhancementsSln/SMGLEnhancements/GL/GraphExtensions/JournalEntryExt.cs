using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.FS;
using PX.Objects.GL;

namespace SMGLEnhancements.GL
{
    public class JournalEntryExt : PXGraphExtension<JournalEntry>
    {
        public static bool IsActive() => true;

        protected virtual void _(Events.RowPersisted<Batch> args)
        {
            if (args is not { Operation: PXDBOperation.Delete, TranStatus: PXTranStatus.Completed })
            {
                return;
            }

            Update<FSAppointment>
                .Set<FSAppointmentExt.usrBatchNbr.EqualTo<Null>>
                .Where<FSAppointmentExt.usrBatchNbr.IsEqual<P.AsString>>.Update(Base, args.Row.BatchNbr);
        }
    }
}
