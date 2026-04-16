import { useState, useCallback } from 'react';
import { toast } from 'react-toastify';

interface ApiCallState<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
}

export function useApiCall<T>() {
  const [state, setState] = useState<ApiCallState<T>>({
    data: null,
    loading: false,
    error: null,
  });

  const execute = useCallback(async (fn: () => Promise<T>, successMessage?: string) => {
    setState({ data: null, loading: true, error: null });
    try {
      const result = await fn();
      setState({ data: result, loading: false, error: null });
      if (successMessage) {
        toast.success(successMessage);
      }
      return result;
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'An error occurred';
      setState({ data: null, loading: false, error: errorMsg });
      toast.error(errorMsg);
      throw err;
    }
  }, []);

  return { ...state, execute };
}
