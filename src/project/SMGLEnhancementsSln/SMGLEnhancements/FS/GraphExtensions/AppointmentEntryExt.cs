using PX.Common;
using PX.Data;
using PX.Objects.FS;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Linq;

namespace SMGLEnhancements.FS.GraphExtensions
{
    public class AppointmentEntryExt : PXGraphExtension<AppointmentEntry>
    {
        public static bool IsActive() => true;

        public FSSetupExt FSSetupExt => Base.SetupRecord.Current.GetExtension<FSSetupExt>();
        public PXAction<FSAppointment> PostLaborToGL;

        [PXButton(CommitChanges = true, Connotation = PX.Data.WorkflowAPI.ActionConnotation.Success)]
        [PXUIField(DisplayName = "Post Labor to GL", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void postLaborToGL()
        {
            if (Base.AppointmentRecords.Current is FSAppointment appointment)
            {
                CreateGLBatch(appointment);
            }
        }

        public void CreateGLBatch(FSAppointment appointment)
        {
            var appointmentLines = Base.AppointmentDetails.Select().FirstTableItems;

            var graph = PXGraph.CreateInstance<JournalEntry>();

            Batch batch = graph.BatchModule.Insert(new Batch
            {
                Module = BatchModule.GL,
                BranchID = appointment.BranchID,
            });

            decimal totalDebit = 0;
            foreach (var appointmentLine in appointmentLines)
            {
                var inventoryItem = InventoryItem.PK.Find(graph, appointmentLine.InventoryID);
                if (appointmentLine.CuryLineAmt.GetValueOrDefault() == 0 || inventoryItem == null)
                {
                    continue;
                }

                InsertDebit(appointment, graph, appointmentLine, inventoryItem);

                totalDebit += appointmentLine.CuryLineAmt.GetValueOrDefault();
            }

            InsertCredit(appointment, graph, totalDebit);

            graph.Persist();

            if (graph.glsetup.Current.AutoPostOption == true)
            {
                graph.release.Press();
            }

            UpdateBatchNbr(graph.BatchModule.Current.BatchNbr);
        }

        private void UpdateBatchNbr(string batchNbr)
        {
            Base.AppointmentRecords.Cache.SetValue<FSAppointmentExt.usrBatchNbr>(Base.AppointmentRecords.Current, batchNbr);
            Base.AppointmentRecords.UpdateCurrent();
            Base.Persist();
        }

        private void InsertCredit(FSAppointment appointment, JournalEntry graph, decimal totalDebit)
        {
            graph.GLTranModuleBatNbr.Insert(new GLTran
            {
                BranchID = appointment.BranchID,
                ProjectID = appointment.ProjectID,
                RefNbr = appointment.RefNbr,
                AccountID = FSSetupExt.UsrServiceLaborBalancingAccount,
                SubID = FSSetupExt.UsrServiceLaborBalancingSub,
                CuryCreditAmt = totalDebit
            });
        }

        private void InsertDebit(FSAppointment appointment, JournalEntry graph, FSAppointmentDet appointmentLine, InventoryItem inventoryItem)
        {
            graph.GLTranModuleBatNbr.Insert(new GLTran
            {
                BranchID = appointment.BranchID,
                RefNbr = appointment.RefNbr,
                ProjectID = appointment.ProjectID,
                AccountID = inventoryItem.COGSAcctID,
                SubID = inventoryItem.COGSSubID,
                Qty = appointmentLine.ActualQty.GetValueOrDefault(),
                CuryDebitAmt = appointmentLine.CuryLineAmt,
                TranDesc = appointmentLine.TranDesc,
            });
        }

        private bool CanPostLaborToGL(FSAppointment appointment) =>
               appointment.Status == ID.Status_Route.CLOSED
            && string.IsNullOrEmpty(appointment.GetExtension<FSAppointmentExt>().UsrBatchNbr)
            && PXContext.PXIdentity.User.IsInRole(FSSetupExt.UsrRoleToPostLabor)
            && ContainsServiceLineWithStaff();



        [PXOverride]
        public virtual IEnumerable CloseAppointment(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseMethod)
        {
            if (Base.AppointmentRecords.Current is FSAppointment appointment)
            {
                ApplyRounding(appointment);
            }

            return baseMethod.Invoke(adapter);
        }

        private void ApplyRounding(FSAppointment appointment)
        {
            var lines = Base.AppointmentDetails.Select().FirstTableItems;
            foreach (FSAppointmentDet line in lines)
            {
                if (line.ActualDuration <= 0)
                {
                    continue;
                }

                line.ActualDuration = Round(line.ActualDuration.Value, 15);
                Base.AppointmentDetails.Update(line);
            }

            int Round(int value, int step)
            {
                return (int)(Math.Ceiling(value / (double)step) * step);
            }
        }

        protected virtual void _(Events.RowSelected<FSAppointment> args, PXRowSelected baseMethod)
        {
            baseMethod.Invoke(args.Cache, args.Args);

            if (args.Row is null)
            {
                return;
            }

            PostLaborToGL.SetVisible(CanPostLaborToGL(args.Row));
            Base.uncloseAppointment.SetEnabled(Base.uncloseAppointment.GetEnabled() && string.IsNullOrEmpty(args.Row.GetExtension<FSAppointmentExt>().UsrBatchNbr));
        }

        public bool ContainsServiceLineWithStaff()
        {
            var employees = Base.AppointmentServiceEmployees
                .Select()
                .FirstTableItems
                .Select(d => d.ServiceLineRef)
                .ToHashSet();

            return Base.AppointmentDetails
                .Select()
                .FirstTableItems
                .Any(line =>
                   line.LineType == ID.LineType_ALL.SERVICE
                   && employees.Contains(line.LineRef));
        }
    }
}
