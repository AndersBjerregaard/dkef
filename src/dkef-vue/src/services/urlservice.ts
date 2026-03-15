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

function updateContact(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contacts/${guid}`
  }
}

function deleteContact(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contacts/${guid}`
  }
}

function getContactAuthorize(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contacts/${guid}/authorize`
  }
}

function getEventPresignedUrl(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/bucket/events/${guid}`
  }
}

function postEvent() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/events`
  }
}

function updateEvent(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/events/${guid}`
  }
}

function deleteEvent(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/events/${guid}`
  }
}

function getEvents() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/events`
  }
}

function getEvent(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/events/${guid}`
  }
}

function getNewsPresignedUrl(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/bucket/news/${guid}`
  }
}

function postNews() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/news`
  }
}

function updateNews(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/news/${guid}`
  }
}

function deleteNews(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/news/${guid}`
  }
}

function getNews() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/news`
  }
}

function getNewsItem(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/news/${guid}`
  }
}

function getGeneralAssemblyPresignedUrl(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/bucket/general-assemblies/${guid}`
  }
}

function postGeneralAssembly() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/general-assemblies`
  }
}

function updateGeneralAssembly(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/general-assemblies/${guid}`
  }
}

function deleteGeneralAssembly(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/general-assemblies/${guid}`
  }
}

function getGeneralAssemblies() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/general-assemblies`
  }
}

function getGeneralAssembly(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/general-assemblies/${guid}`
  }
}

function getFeed() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/feed`
  }
}

export default {
  getContacts,
  updateContact,
  deleteContact,
  getContactAuthorize,
  getEventPresignedUrl,
  postEvent,
  updateEvent,
  deleteEvent,
  getEvents,
  getEvent,
  getNewsPresignedUrl,
  postNews,
  updateNews,
  deleteNews,
  getNews,
  getNewsItem,
  getGeneralAssemblyPresignedUrl,
  postGeneralAssembly,
  updateGeneralAssembly,
  deleteGeneralAssembly,
  getGeneralAssemblies,
  getGeneralAssembly,
  getFeed,
}
