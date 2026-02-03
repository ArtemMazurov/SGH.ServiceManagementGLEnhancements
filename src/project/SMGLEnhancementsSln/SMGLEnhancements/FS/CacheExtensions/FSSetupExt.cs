using PX.Data;
using PX.Data.BQL;
using PX.SM;
using PX.Objects.GL;
using PX.Objects.FS;

namespace SMGLEnhancements.FS
{
    public sealed class FSSetupExt : PXCacheExtension<FSSetup>
    {
        public static bool IsActive() => true;

        #region UsrServiceLaborBalancingAccount
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Service Labor Balancing Account", Required = true)]
        [PXSelector(typeof(Account.accountID),
            DescriptionField = typeof(Account.description),
            SubstituteKey = typeof(Account.accountCD))]
        public int? UsrServiceLaborBalancingAccount { get; set; }
        public abstract class usrServiceLaborBalancingAccount : BqlInt.Field<usrServiceLaborBalancingAccount> { }
        #endregion

        #region UsrServiceLaborBalancingSub
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Service Labor Balancing Sub", Required = true)]
        [PXSelector(typeof(Sub.subID),
            DescriptionField = typeof(Sub.description),
            SubstituteKey = typeof(Sub.subCD))]
        public int? UsrServiceLaborBalancingSub { get; set; }
        public abstract class usrServiceLaborBalancingSub : BqlInt.Field<usrServiceLaborBalancingSub> { }
        #endregion

        #region UsrRoleToPostLabor
        [PXDBString(64, IsUnicode = true)]
        [PXUIField(DisplayName = "Role To Post To GL Service Labor")]
        [PXSelector(typeof(Roles.rolename), DescriptionField = typeof(Roles.descr))]
        public string UsrRoleToPostLabor { get; set; }
        public abstract class usrRoleToPostLabor : BqlString.Field<usrRoleToPostLabor> { }
        #endregion
    }
}
