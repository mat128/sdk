﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.NuGetPackageDownloader;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.Workloads.Workload.Install;
using Microsoft.Extensions.EnvironmentAbstractions;
using Microsoft.NET.Sdk.WorkloadManifestReader;
using NuGet.Common;

namespace Microsoft.DotNet.Workloads.Workload
{
    internal static class WorkloadIntegrityChecker
    {
        public static void RunFirstUseCheck()
        {
            var creationParameters = new WorkloadResolverFactory.CreationParameters()
            {
                CheckIfFeatureBandManifestExists = true,
                UseInstalledSdkVersionForResolver = true
            };

            var creationResult = WorkloadResolverFactory.Create(creationParameters);
            var sdkFeatureBand = new SdkFeatureBand(creationResult.SdkVersion);
            var verifySignatures = WorkloadCommandBase.ShouldVerifySignatures();
            var tempPackagesDirectory = new DirectoryPath(Path.Combine(PathUtilities.CreateTempSubdirectory(), "dotnet-sdk-advertising-temp"));
            var packageDownloader = new NuGetPackageDownloader(
                tempPackagesDirectory,
                verboseLogger: new NullLogger(), // TODO: Maybe use default value instead.
                verifySignatures: verifySignatures);

            var installer = WorkloadInstallerFactory.GetWorkloadInstaller(
                Reporter.Output, sdkFeatureBand, creationResult.WorkloadResolver, VerbosityOptions.normal, creationResult.UserProfileDir, verifySignatures, packageDownloader, creationResult.DotnetPath);
            var repository = installer.GetWorkloadInstallationRecordRepository();

            CliTransaction.RunNew(context => installer.InstallWorkloads(repository.GetInstalledWorkloads(sdkFeatureBand), sdkFeatureBand, context));
        }
    }
}
