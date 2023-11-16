export interface UsageData {
    finish_reason: string;
    completion_tokens: number;
    prompt_tokens: number;
    total_tokens: number;
    response_time: number;
}