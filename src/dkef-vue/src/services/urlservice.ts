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

function getEventPresignedUrl(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!';
    default:
      return `bucket/events/${guid}`
  }
}

export default {
  getContacts,
  getEventPresignedUrl
}
