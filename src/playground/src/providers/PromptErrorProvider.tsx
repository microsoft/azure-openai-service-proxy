import { PropsWithChildren, createContext, useContext, useState } from "react";

export type PromptErrorValue = {
  promptError?: string;
  setPromptError: React.Dispatch<string | undefined>;
};

const PromptErrorContext = createContext<PromptErrorValue>({
  promptError: undefined,
  setPromptError: () => {},
});

const PromptErrorProvider: React.FC<PropsWithChildren> = ({ children }) => {
  const [promptError, setPromptError] = useState<string | undefined>(undefined);

  return (
    <PromptErrorContext.Provider value={{ promptError, setPromptError }}>
      {children}
    </PromptErrorContext.Provider>
  );
};

const usePromptErrorContext = () => useContext(PromptErrorContext);

// eslint-disable-next-line react-refresh/only-export-components
export { PromptErrorProvider, usePromptErrorContext };
