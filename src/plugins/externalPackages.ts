export const externalPackageManifestFileName = "architecture-studio.package.json" as const;

export const externalPackageStatusKinds = ["Loaded", "Invalid"] as const;
export const externalPackageContributionKinds = ["Standards", "Compliance", "Templates", "Graph"] as const;

export type ExternalPackageStatusKind = (typeof externalPackageStatusKinds)[number];
export type ExternalPackageContributionKind = (typeof externalPackageContributionKinds)[number];

export type ExternalPackageManifest = {
  readonly id: string;
  readonly version: string;
  readonly schemaVersion: string;
  readonly displayName: string;
  readonly contributions: {
    readonly standardsPackages?: readonly string[];
    readonly regulations?: readonly string[];
    readonly controls?: readonly string[];
    readonly templates?: readonly string[];
    readonly graphDatasets?: readonly string[];
  };
};

export type ExternalPackageLoadStatus = {
  readonly packageId: string;
  readonly status: ExternalPackageStatusKind;
  readonly message: string;
  readonly contributionKinds: readonly ExternalPackageContributionKind[];
};

export function getExternalPackageContributionKinds(
  manifest: ExternalPackageManifest
): readonly ExternalPackageContributionKind[] {
  const contributionKinds: ExternalPackageContributionKind[] = [];

  if ((manifest.contributions.standardsPackages?.length ?? 0) > 0) {
    contributionKinds.push("Standards");
  }

  if ((manifest.contributions.regulations?.length ?? 0) > 0 || (manifest.contributions.controls?.length ?? 0) > 0) {
    contributionKinds.push("Compliance");
  }

  if ((manifest.contributions.templates?.length ?? 0) > 0) {
    contributionKinds.push("Templates");
  }

  if ((manifest.contributions.graphDatasets?.length ?? 0) > 0) {
    contributionKinds.push("Graph");
  }

  return contributionKinds;
}
