window.webauthn = {

    registerPasskey: async function (optionsJson) {
        if (!window.PublicKeyCredential || typeof window.PublicKeyCredential !== "function") {
            throw new Error("WebAuthn is not supported in this browser.");
        }

        const options = typeof optionsJson === "string"
            ? JSON.parse(optionsJson)
            : optionsJson;

        const publicKey = window.PublicKeyCredential.parseCreationOptionsFromJSON
            ? window.PublicKeyCredential.parseCreationOptionsFromJSON(options)
            : coerceCreationOptions(options);

        let credential;
        try {
            credential = await navigator.credentials.create({ publicKey });
        } catch (error) {
            if (error && error.name === "NotAllowedError") {
                throw new Error("Passkey registration was cancelled or denied.");
            }
            throw error;
        }

        if (!credential) {
            throw new Error("No credential was created.");
        }

        return credentialToAttestationResponse(credential);
    },


    loginPasskey: async function (optionsJson) {
        if (!window.PublicKeyCredential || typeof window.PublicKeyCredential !== "function") {
            throw new Error("WebAuthn is not supported in this browser.");
        }

        const options = typeof optionsJson === "string"
            ? JSON.parse(optionsJson)
            : optionsJson;

        const publicKey = window.PublicKeyCredential.parseRequestOptionsFromJSON
            ? window.PublicKeyCredential.parseRequestOptionsFromJSON(options)
            : coerceRequestOptions(options);

        let credential;
        try {
            credential = await navigator.credentials.get({ publicKey });
        } catch (error) {
            if (error && error.name === "NotAllowedError") {
                throw new Error("Passkey login was cancelled or denied.");
            }
            throw error;
        }

        if (!credential) {
            throw new Error("No credential was retrieved.");
        }

        return credentialToAssertionResponse(credential);
    }
};


function coerceCreationOptions(options) {
    const coerced = { ...options };

    if (typeof coerced.challenge === "string") {
        coerced.challenge = base64UrlToBuffer(coerced.challenge);
    }

    if (coerced.user && typeof coerced.user.id === "string") {
        coerced.user = {
            ...coerced.user,
            id: base64UrlToBuffer(coerced.user.id)
        };
    }

    if (Array.isArray(coerced.excludeCredentials)) {
        coerced.excludeCredentials = coerced.excludeCredentials.map(function (descriptor) {
            if (typeof descriptor.id === "string") {
                return {
                    ...descriptor,
                    id: base64UrlToBuffer(descriptor.id)
                };
            }
            return descriptor;
        });
    }
    return coerced;
}

function credentialToAttestationResponse(credential) {
    if (typeof credential.toJSON === "function") {
        return credential.toJSON();
    }

    return {
        id: credential.id,
        rawId: bufferToBase64Url(credential.rawId),
        type: credential.type,
        authenticatorAttachment: credential.authenticatorAttachment,
        clientExtensionResults: credential.getClientExtensionResults(),
        response: {
            attestationObject: bufferToBase64Url(credential.response.attestationObject),
            clientDataJSON: bufferToBase64Url(credential.response.clientDataJSON),
            transports: credential.response.getTransports
                ? credential.response.getTransports()
                : undefined
        }
    };
}


function coerceRequestOptions(options) {
    const coerced = { ...options };

    if (typeof coerced.challenge === "string") {
        coerced.challenge = base64UrlToBuffer(coerced.challenge);
    }

    if (Array.isArray(coerced.allowCredentials)) {
        coerced.allowCredentials = coerced.allowCredentials.map(function (descriptor) {
            if (typeof descriptor.id === "string") {
                return {
                    ...descriptor,
                    id: base64UrlToBuffer(descriptor.id)
                };
            }
            return descriptor;
        });
    }
    return coerced;
}

function credentialToAssertionResponse(credential) {
    if (typeof credential.toJSON === "function") {
        return credential.toJSON();
    }

    return {
        id: credential.id,
        rawId: bufferToBase64Url(credential.rawId),
        type: credential.type,
        authenticatorAttachment: credential.authenticatorAttachment,
        clientExtensionResults: credential.getClientExtensionResults(),
        response: {
            authenticatorData: bufferToBase64Url(credential.response.authenticatorData),
            clientDataJSON: bufferToBase64Url(credential.response.clientDataJSON),
            signature: bufferToBase64Url(credential.response.signature),
            userHandle: credential.response.userHandle
                ? bufferToBase64Url(credential.response.userHandle)
                : undefined
        }
    };
}


function base64UrlToBuffer(value) {
    const base64 = value.replace(/-/g, "+").replace(/_/g, "/");
    const padded = base64 + "=".repeat((4 - (base64.length % 4)) % 4);
    const binary = atob(padded);
    const bytes = new Uint8Array(binary.length);

    for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
    }

    return bytes.buffer;
}

function bufferToBase64Url(buffer) {
    const bytes = new Uint8Array(buffer);
    let binary = "";

    for (let i = 0; i < bytes.length; i++) {
        binary += String.fromCharCode(bytes[i]);
    }

    return btoa(binary)
        .replace(/\+/g, "-")
        .replace(/\//g, "_")
        .replace(/=+$/g, "");
}