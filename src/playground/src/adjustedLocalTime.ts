export const adjustedLocalTime = (
  timestamp: Date,
  utcOffsetInMinutes: number
): string => {
  // returns time zone adjusted date/time
  const date = new Date(timestamp);
  // get the timezone offset component that was added as no tz supplied in date time
  const tz = date.getTimezoneOffset();
  // remove the browser based timezone offset
  date.setMinutes(date.getMinutes() - tz);
  // add the event timezone offset
  date.setMinutes(date.getMinutes() - utcOffsetInMinutes);

  // Get the browser locale
  const locale = navigator.language || navigator.languages[0];

  // Specify the formatting options
  const options: Intl.DateTimeFormatOptions = {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "numeric",
    minute: "numeric",
  };

  // Create an Intl.DateTimeFormat object
  const formatter = new Intl.DateTimeFormat(locale, options);
  // Format the date
  const formattedDate = formatter.format(date);
  return formattedDate;
};
