const mode = import.meta.env.VITE_MODE

console.info('Environment Mode loaded as: ', mode)

function getContacts() {
  switch (mode) {
    case 'Development':
      return '/contacts.json'
    default:
      return '/contacts'
  }
}

function updateContact(guid: String) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!';
    default:
      return `/contacts/${guid}`
  }
}

function getContactAuthorize(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!';
    default:
      return `/contacts/${guid}/authorize`
  }
}

function getEventPresignedUrl(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!';
    default:
      return `/bucket/events/${guid}`
  }
}

function postEvent() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!';
    default:
      return `/events`
  }
}

function getEvents() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!';
    default:
      return `/events`
  }
}

function getEvent(guid: String) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!';
    default:
      return `/events/${guid}`
  }
}

export default {
  getContacts,
  updateContact,
  getContactAuthorize,
  getEventPresignedUrl,
  postEvent,
  getEvents,
  getEvent
}
