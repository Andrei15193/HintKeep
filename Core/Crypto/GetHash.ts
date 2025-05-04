export type HashAlgorithm = "SHA-256" | "SHA-384" | "SHA-512";

export async function getHashAsync(value: string, algorith: HashAlgorithm): Promise<string> {
    const encoder = new TextEncoder();
    const data = encoder.encode(value);

    const hashBuffer = await window.crypto.subtle.digest(algorith, data);

    const hash = new Uint8Array(hashBuffer)
        .reduce(
            (result, byte) => {
                result.push(
                    byte
                        .toString(16)
                        .padStart(2, "0")
                );

                return result;
            },
            new Array<string>()
        )
        .join("");

    return hash;
}