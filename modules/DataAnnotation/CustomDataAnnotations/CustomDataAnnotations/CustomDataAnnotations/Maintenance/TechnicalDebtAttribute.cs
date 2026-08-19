using System;
using System.Collections.Generic;
using System.Text;

namespace CustomDataAnnotations.Maintenance
{
    public enum CategoryType
    {
        /// <summary>
        /// Code smell. The inner implementation is too smell, suggesting to copy-and-paste it to other API and refactor it in that other API.
        /// </summary>
        CodeSmell,
        /// <summary>
        /// Naming convention issue
        /// </summary>
        NamingIssue,
        /// <summary>
        /// Namespace issue
        /// </summary>
        NamespaceIssue,
        /// <summary>
        /// Bad design that pass many arguments (not pass a model or a bean)
        /// </summary>
        PrimitiveObsessionIssue,
        /// <summary>
        /// Bad design that indicates that parameter of a method is NOT a good design covention in modern.
        /// </summary>
        ParameterOrderingIssue,

        /// <summary>
        /// To optimize the performance (time performance and space issue), use different strategy to achieve the same functionality.
        /// </summary>
        DifferentStrategyIssue,

        /// <summary>
        /// API that invokes that API belongs to deprecated .NET Framework.
        /// </summary>
        DeprecatedApiOfOutdatedFrameworkIssue,
        /// <summary>
        /// API that invokes API that belongs to deprecated dependency.
        /// </summary>
        DeprecatedApiOfOutdatedDependencyIssue,
        /// <summary>
        /// API that invokes deprecated API.
        /// </summary>
        DeprecatedApiOfOutdatedApiIssue,

        /// <summary>
        /// Needs of legacy solution change, so deprecated it and write a new one for safety (avoid unexpected behavior in other functionalities)
        /// </summary>
        NeedsChanged,

        /// <summary>
        /// Indicates that time precision will be lost.
        /// </summary>
        TimePrecisionLossIssue,

        /// <summary>
        /// Indicates that precision of memory calucation will be lost.
        /// </summary>
        MemoryPrecisionLossIssue,

        /// <summary>
        /// Indicates that there is an non-zero allocation during the API call
        /// </summary>
        NonZeroAllocationIssue,

        /// <summary>
        /// Indicates that this API is highly coupled.
        /// </summary>
        HighCouplingIssue,

        /// <summary>
        /// Indicates that this API violates SRP (Single Response Principle).
        /// </summary>
        ViolateSrpIssue,

        /// <summary>
        /// Indicates that API is in low rigidity.
        /// </summary>
        LowRigidityIssue,

        /// <summary>
        /// Indicates that it violates DRY (Don't repeat it yourself) during this API call
        /// </summary>
        ViolateDryPrincipleIssue,

        /// <summary>
        /// Instable behavior 
        /// </summary>
        InstableBehaviorIssue,

        /// <summary>
        /// Instable behavior occurred in multiple thread in different devices.
        /// </summary>
        InstableBehaviorInDifferentDevicesIssue,

        /// <summary>
        /// Instable behavior occurred in multiple thread in same device.
        /// </summary>
        InstableBehaviorInMultipleThreadsIssue,

        /// <summary>
        /// Instable behavior occurred in different OS platform (e.g. in Windows, the API returns A, but at same time, in Linux, the API return B)
        /// </summary>
        InstableBehaviorInDiffentOSPlatformIssue,

        /// <summary>
        /// Instable behavior occurred in Windows platform. (e.g. in Windows, the API returns A, but in a few days, in Windows, the API return B)
        /// </summary>
        InstableBehaviorInWindows,

        /// <summary>
        /// Instable behavior occurred in Linux platform. (e.g. in Linux, the API returns A, but in a few days, in Linux, the API return B)
        /// </summary>
        InstableBehaviorInLinux,

        /// <summary>
        /// Instable behavior occurred in Mac OS platform. (e.g. in Mac OS, the API returns A, but in a few days, in Mac OS, the API return B)
        /// </summary>
        InstableBehaviorInMacOs,

        /// <summary>
        /// Instable behavior occurred when other programming language interop with this API  (e.g. An script in Python 3+ invokes the API written in `C#`, and its API behaves unexpectedly. 
        /// </summary>
        InstableBehaviorWhenInterop,

        /// <summary>
        /// Instable behavior occurred due to marshalling.
        /// </summary>
        InstableBehaviorDueToMarshalling,

        /// <summary>
        /// Memory leak issue
        /// </summary>
        MemoryLeakIssue,

        /// <summary>
        /// Time Performance Issue
        /// </summary>
        ExecutedTimePerformanceIssue,

        /// <summary>
        /// Space Issue
        /// </summary>
        MemoryPerformanceIssue,
        /// <summary>
        /// Security Issue
        /// </summary>
        SecurityVulnerability,
        /// <summary>
        /// Marks the API is in outdated dependency
        /// </summary>
        OutdatedDependency,

        /// <summary>
        /// Marks the API is in outdated framework
        /// </summary>
        OutdatedFramework,

        /// <summary>
        /// Marks the API uses the outdated strategy or approach.
        /// </summary>
        OutdatedStrategy,

        /// <summary>
        /// Marks the API is hard to mock.
        /// </summary>
        MockingIssue,

        /// <summary>
        /// Marks the API is hard to be unit tested.
        /// </summary>
        UnitTestIssue,

        /// <summary>
        /// Other issue
        /// </summary>
        OtherIssue
    }

    [AttributeUsage(AttributeTargets.All)]
    public class TechnicalDebtAttribute : Attribute
    {

        public CategoryType Category { get; }
        public string BetterAlternative { get; }

        public TechnicalDebtAttribute(CategoryType category, string betterAlternative = "")
        {
            Category = category;
            BetterAlternative = betterAlternative;
        }
    }
}
