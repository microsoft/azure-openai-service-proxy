import { useEventDataContext } from "../EventDataProvider";

export const Image = () => {
  const { eventCode } = useEventDataContext();

  return (
    <>
      <p>Placeholder</p>
    </>
  );
};
