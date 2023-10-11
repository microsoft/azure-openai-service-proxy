export async function callApi(data: any, eventCode: string): Promise<any> {
    try {
        const response = await fetch(
            'https://openai-proxy-23uljr-ca.salmonsea-82a61dba.swedencentral.azurecontainerapps.io/api/oai_prompt', {
                method: 'POST',
                headers: {
                    'accept': 'application/json',
                    'Content-Type': 'application/json',
                    'openai-event-code': eventCode
                },
                body: JSON.stringify(data)
            }
        );
        if (response.status === 200) {
            const responseanswer = await response.json();
            return { answer: responseanswer, status: response.status };
        } else if (response.status === 401) {
            const responseanswer = await response.json();
            alert(`Unauthorized: ${response.status}`);
            return { answer: responseanswer, status: response.status };
        } else if (response.status === 400 || response.status === 404 || response.status === 415) {
            const responseanswer = await response.json();
            alert(`Bad Request: ${responseanswer.assistant.content}`);
            return { answer: responseanswer, status: response.status };
        } else if (response.status === 403) {
            const responseanswer = await response.json();
            alert(`Permission Error: ${response.status}`);
            return { answer: responseanswer, status: response.status };
        } else if (response.status === 409) {
            const responseanswer = await response.json();
            alert(`Try Again: ${response.status}`);
            return { answer: responseanswer, status: response.status };
        } else if (response.status === 429) {
            const responseanswer = await response.json();
            alert(`Rate Limited: ${response.status}`);
            return { answer: responseanswer, status: response.status };
        } else if (response.status === 500) {
            const responseanswer = await response.json();
            alert(`Internal Server Error: ${response.status}`);
            return { answer: responseanswer, status: response.status };
        } else {
            const responseanswer = await response.json();
            alert(`HTTP error! status: ${response.status}`);
            return { answer: responseanswer, status: response.status };
        }
    } catch (error) {
        console.error('Error calling API:', error);
        return error;
    }
}
