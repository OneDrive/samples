import { useEffect, useState } from "react";

/**
 * Hook used to reduce the number of updates broadcast from a rapidly updating external value
 * 
 * @see https://usehooks.com/useDebounce/
 * 
 * @param value The rapidly changing value to track
 * @param delay The delay to use before updating the debounced value (milliseconds)
 */
export default function useDebounce<T>(value: T, delay = 500): T {

    const [debouncedValue, setDebouncedValue] = useState(value);

    useEffect(() => {

        const handler = setTimeout(() => {
            setDebouncedValue(value);
        }, delay);

        return () => {
            clearTimeout(handler);
        };

    }, [value, delay]);

    return debouncedValue;
}
