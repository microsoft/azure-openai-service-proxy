


export async function callApi(data: any): Promise<any> {
    try {
        const response = await fetch(
            'https://openai-proxy-23uljr-ca.salmonsea-82a61dba.swedencentral.azurecontainerapps.io/api/oai_prompt', {
                method: 'POST',
                headers: {
                    'accept': 'application/json',
                    'Content-Type': 'application/json',
                    'x-ms-client-principal': 'ewogICJpZGVudGl0eVByb3ZpZGVyIjogImdpdGh1YiIsCiAgInVzZXJJZCI6ICJSbXRgNTdbYktEJHtRbiNqVXhNNiV1Lk5DZjtyRlBTQUdfOH4iLAogICJ1c2VyRGV0YWlscyI6ICJ1c2VybmFtZSIsCiAgInVzZXJSb2xlcyI6IFsiYW5vbnltb3VzIiwgImF1dGhlbnRpY2F0ZWQiXSwKICAiY2xhaW1zIjogW3sKICAgICJ0eXAiOiAibmFtZSIsCiAgICAidmFsIjogIkF6dXJlIFN0YXRpYyBXZWIgQXBwcyIKICB9XQp9'
                },
                body: JSON.stringify(data)
            }
        );
        if (response.status === 200) {
            const responseData = await response.json();
            return responseData;
        } else if (response.status === 401) {
            throw new Error(`Unauthorized: ${response.status}`);
        } else if (response.status === 500) {
            throw new Error(`Internal Server Error: ${response.status}`);
        } else {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
    } catch (error) {
        console.error('Error calling API:', error);
        throw error;
    }
}
