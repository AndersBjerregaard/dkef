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

function postContact() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return '/contacts'
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
      return `/contents/events`
  }
}

function updateEvent(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contents/events/${guid}`
  }
}

function deleteEvent(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contents/${guid}`
  }
}

function getEvents() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contents/events`
  }
}

function getEvent(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contents/events/${guid}`
  }
}

function postEventSignUp(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contents/events/${guid}/sign-ups`
  }
}

function getMyEventSignUpStatus(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contents/events/${guid}/sign-ups/me`
  }
}

function getEventSignUps(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contents/events/${guid}/sign-ups`
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
      return `/contents/news`
  }
}

function updateNews(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contents/news/${guid}`
  }
}

function deleteNews(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contents/${guid}`
  }
}

function getNews() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contents/news`
  }
}

function getNewsItem(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contents/news/${guid}`
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
      return `/contents/general-assemblies`
  }
}

function updateGeneralAssembly(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contents/general-assemblies/${guid}`
  }
}

function deleteGeneralAssembly(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contents/${guid}`
  }
}

function getGeneralAssemblies() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contents/general-assemblies`
  }
}

function getGeneralAssembly(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contents/general-assemblies/${guid}`
  }
}

function getFeed() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/contents`
  }
}

function postContactMessage() {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/information`
  }
}

function getAttachmentPresignedUrl(guid: string) {
  switch (mode) {
    case 'Development':
      throw 'Unimplemented!'
    default:
      return `/bucket/attachments/${guid}`
  }
}

function getNexiPocSession() {
  switch (mode) {
    case 'Development':
      return '/payments/nexi/poc-session'
    default:
      return '/payments/nexi/poc-session'
  }
}

export default {
  getContacts,
  updateContact,
  deleteContact,
  postContact,
  getContactAuthorize,
  getEventPresignedUrl,
  postEvent,
  updateEvent,
  deleteEvent,
  getEvents,
  getEvent,
  postEventSignUp,
  getMyEventSignUpStatus,
  getEventSignUps,
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
  postContactMessage,
  getAttachmentPresignedUrl,
  getNexiPocSession,
}
