import { PropsWithChildren, createContext, useContext, useState } from "react";

export type PromptErrorValue = {
  promptError?: string;
  setPromptError: React.Dispatch<string | undefined>;
};

const PromptErrorContext = createContext<PromptErrorValue>({
  promptError: undefined,
  setPromptError: (error: string | undefined) => {},
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

export { PromptErrorProvider, usePromptErrorContext };
