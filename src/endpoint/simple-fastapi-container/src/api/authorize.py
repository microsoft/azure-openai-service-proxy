""" Authorize a user to access playground based specific time bound event."""

import string


class Authorize:
    """Authorizes a user to access a specific time bound event."""

    def __init__(self, connection_string) -> None:
        self.connection_string = connection_string

    def authorize(self, event_code: str) -> bool:
        """Authorizes a user to access a specific time bound event."""
        # async lookup of event_code in azure storage table

        # check event_code is not empty
        if not event_code:
            return False

        if not 6 < len(event_code) < 20:
            return False

        # check event code is only printable characters
        if not all(c in string.printable for c in event_code):
            return False

        if event_code == "advocacy202310":
            return True

        # if event_code is not found in azure storage table

        return False
