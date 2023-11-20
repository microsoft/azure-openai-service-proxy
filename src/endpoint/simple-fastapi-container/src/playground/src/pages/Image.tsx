import { useEventDataContext } from "../providers/EventDataProvider";

export const Image = () => {
  const { eventCode } = useEventDataContext();

  return (
    <>
      <p>Placeholder</p>
    </>
  );
};
