namespace _MicroserviceTemplate_.WebApi.Authorization
{
    /// <summary>
    /// _MicroserviceTemplate_ policies.
    /// </summary>
    public static class _MicroserviceTemplate_Policies
    {
        /// <summary>
        /// Read my tables policy.
        /// </summary>
        public const string MYTABLES_READ = "mytables-read";

        /// <summary>
        /// Add my tables policy.
        /// </summary>
        public const string MYTABLES_ADD = "mytables-add";

        /// <summary>
        /// Update my tables policy.
        /// </summary>
        public const string MYTABLES_UPDATE = "mytables-update";

        /// <summary>
        /// Delete my tables policy.
        /// </summary>
        public const string MYTABLES_DELETE = "mytables-delete";

        /// <summary>
        /// Execute job policy.
        /// </summary>
        public const string JOB_EXECUTE = "job-execute";
    }
}