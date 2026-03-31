const DEFAULT_API_BASE_URL = "http://localhost:5256";

function buildApiUrl(path) {
    const normalizedPath = path.startsWith("/") ? path : `/${path}`;
    return `${getApiBaseUrl()}${normalizedPath}`;
}
function getApiBaseUrl() {
    return DEFAULT_API_BASE_URL;
}

async function parseResponse(response) {
    if (response.status === 204) {
        return null;
    }

    const contentType = response.headers.get("content-type") || "";
    if (contentType.includes("application/json")) {
        return response.json();
    }

    return response.text();
}

function extractErrorMessage(data, status) {
    const messages = extractErrorMessages(data);
    if (messages.length > 0) {
        return messages.join("\n");
    }

    return `Request failed with status ${status}`;
}

function extractErrorMessages(data) {
    if (typeof data === "string" && data.trim().length > 0) {
        return [data];
    }

    if (data && typeof data === "object") {
        if (typeof data.message === "string") {
            return [data.message];
        }

        if (data.errors && typeof data.errors === "object") {
            return Object.entries(data.errors)
                .flatMap(([field, errorList]) => {
                    if (!Array.isArray(errorList) || errorList.length === 0) {
                        return [];
                    }

                    const label = getFieldLabel(field);
                    return errorList.map((message) => `${label}: ${String(message)}`);
                });
        }
    }

    return [];
}

function getFieldLabel(field) {
    const labels = {
        FirstName: "Nombre",
        LastName: "Apellidos",
        DocumentType: "Tipo de documento",
        DocumentId: "Numero de documento",
        Email: "Email",
        Phone: "Telefono",
        Country: "Pais",
    };

    return labels[field] || field;
}

export async function apiRequest(path, options = {}) {
    const { method = "GET", body, headers = {} } = options;
    const response = await fetch(buildApiUrl(path), {
        method,
        headers: {
            Accept: "application/json",
            ...(body !== undefined ? { "Content-Type": "application/json" } : {}),
            ...headers,
        },
        body: body !== undefined ? JSON.stringify(body) : undefined,
    });

    const data = await parseResponse(response);

    if (!response.ok) {
        const error = new Error(extractErrorMessage(data, response.status));
        error.details = extractErrorMessages(data);
        throw error;
    }

    return data;
}
