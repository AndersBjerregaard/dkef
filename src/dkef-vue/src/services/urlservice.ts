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

function getNewsPresignedUrl(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!';
    default:
      return `/bucket/news/${guid}`
  }
}

function postNews() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!';
    default:
      return `/news`
  }
}

function getNews() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!';
    default:
      return `/news`
  }
}

function getNewsItem(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!';
    default:
      return `/news/${guid}`
  }
}

function getGeneralAssemblyPresignedUrl(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!';
    default:
      return `/bucket/general-assemblies/${guid}`
  }
}

function postGeneralAssembly() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!';
    default:
      return `/general-assemblies`
  }
}

function getGeneralAssemblies() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!';
    default:
      return `/general-assemblies`
  }
}

function getGeneralAssembly(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!';
    default:
      return `/general-assemblies/${guid}`
  }
}

export default {
  getContacts,
  updateContact,
  getContactAuthorize,
  getEventPresignedUrl,
  postEvent,
  getEvents,
  getEvent,
  getNewsPresignedUrl,
  postNews,
  getNews,
  getNewsItem,
  getGeneralAssemblyPresignedUrl,
  postGeneralAssembly,
  getGeneralAssemblies,
  getGeneralAssembly,
}
