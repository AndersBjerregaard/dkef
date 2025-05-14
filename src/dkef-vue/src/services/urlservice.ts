const mode = import.meta.env.VITE_MODE

console.info('Mode loaded as: ', mode)

function getContacts() {
  switch (mode) {
    case 'Development':
      return '/contacts.json'
    default:
      return '/contacts'
  }
}

export default {
  getContacts,
}
