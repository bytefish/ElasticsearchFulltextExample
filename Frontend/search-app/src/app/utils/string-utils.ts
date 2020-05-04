export class StringUtils {
    
    static isNullOrWhitespace(input: string): boolean {
        return !input || !input.trim();
    }
    
}
