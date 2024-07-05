// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

export class StringUtils {
    
    static isNullOrWhitespace(input: string): boolean {
        return !input || !input.trim();
    }
    
}
