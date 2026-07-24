using NDBS.Xia.Api.Dtos;

namespace Ndbs.MauiToolkit.Xia.MicroApps.Sync
{
    /// <summary>
    /// Merges the <c>$meta</c> (display name / description) and <c>$head</c> (ETag)
    /// API results into a unified <see cref="MicroAppServerImage"/> per micro app,
    /// joining over the micro app name (§3.1). Micro apps that appear in only one of
    /// the two results are still included, with the missing counterpart left empty.
    /// </summary>
    public static class MicroAppServerImageBuilder
    {
        /// <summary>Builds the merged server images.</summary>
        /// <param name="meta">The <c>$meta</c> result (may be <see langword="null"/>).</param>
        /// <param name="etags">The <c>$head</c> result (may be <see langword="null"/>).</param>
        /// <returns>The merged server images keyed by micro app name.</returns>
        public static IReadOnlyList<MicroAppServerImage> Build(
            IReadOnlyList<ProjectResult>? meta,
            IReadOnlyList<ProjectMetadataFile>? etags)
        {
            var metaByName = new Dictionary<string, ProjectResult>(StringComparer.Ordinal);
            if (meta is not null)
            {
                foreach (var project in meta)
                {
                    if (!string.IsNullOrWhiteSpace(project?.Name))
                    {
                        metaByName[project!.Name] = project;
                    }
                }
            }

            var etagByName = new Dictionary<string, string?>(StringComparer.Ordinal);
            if (etags is not null)
            {
                foreach (var head in etags)
                {
                    var name = head?.Identifier?.Name;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        etagByName[name!] = head!.Etag;
                    }
                }
            }

            var names = new HashSet<string>(StringComparer.Ordinal);
            names.UnionWith(metaByName.Keys);
            names.UnionWith(etagByName.Keys);

            var result = new List<MicroAppServerImage>(names.Count);
            foreach (var name in names)
            {
                metaByName.TryGetValue(name, out var project);
                var info = project?.Modes?.FirstOrDefault()?.ProjectInfo;
                etagByName.TryGetValue(name, out var etag);

                result.Add(new MicroAppServerImage(
                    name,
                    info?.DisplayName,
                    info?.Description,
                    etag));
            }

            return result;
        }
    }
}
