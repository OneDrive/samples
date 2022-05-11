/**
 * Combines an arbitrary set of paths ensuring and normalizes the slashes
 *
 * @param paths 0 to n path parts to combine
 */
 export function combine(...paths: (string | null | undefined)[]): string {

    return paths
        .filter(path => typeof path === "string" && path !== null)
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        .map(path => path!.replace(/^[\\|/]/, "").replace(/[\\|/]$/, ""))
        .join("/")
        .replace(/\\/g, "/");
}
