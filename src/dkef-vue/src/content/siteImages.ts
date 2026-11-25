import hero from '@/assets/el-mast.jpeg'
import network from '@/assets/man-studying.png'
import development from '@/assets/corporate-woman.png'
import sections from '@/assets/dkef-sections.png'
import secretariat from '@/assets/secretary.jpg'
import advantagesNetwork from '@/assets/advantages1.jpeg'
import events from '@/assets/advantages2.jpeg'
import mentoring from '@/assets/advantages3.jpeg'
import sparring from '@/assets/advantages4.jpeg'
import magazine from '@/assets/advantages5.jpeg'

export interface SiteImageValue {
  url: string
  alt: string
}

export interface SiteImageSlot extends SiteImageValue {
  id: string
  label: string
  previewClass: string
  aspectRatio?: string
}

export interface SiteImageGroup {
  id: string
  label: string
  slots: readonly SiteImageSlot[]
}

const photo = 'w-full rounded-xl'
const card = 'w-72 max-w-full h-44 object-cover rounded-xl'

export const siteImageGroups: readonly SiteImageGroup[] = [
  {
    id: 'home',
    label: 'Forsiden',
    slots: [
      {
        id: 'hero',
        label: 'Introduktion',
        url: hero,
        alt: 'Elmast',
        previewClass: photo,
        aspectRatio: '2 / 3',
      },
      {
        id: 'network',
        label: 'Netværk',
        url: network,
        alt: 'Mand, der studerer',
        previewClass: photo,
        aspectRatio: '55 / 73',
      },
      {
        id: 'development',
        label: 'Faglig udvikling',
        url: development,
        alt: 'Kvinde på arbejde',
        previewClass: photo,
        aspectRatio: '55 / 73',
      },
    ],
  },
  {
    id: 'about',
    label: 'Om DKEF',
    slots: [
      {
        id: 'sections',
        label: 'Sektionskort',
        url: sections,
        alt: 'Kort over DKEF’s sektioner',
        previewClass: 'w-full h-80 object-contain rounded-xl',
      },
      {
        id: 'secretariat',
        label: 'Sekretariat',
        url: secretariat,
        alt: 'DKEF’s sekretær',
        previewClass: 'w-48 h-48 rounded-full object-cover',
      },
    ],
  },
  {
    id: 'member-advantages',
    label: 'Medlemsfordele',
    slots: [
      {
        id: 'network',
        label: 'Netværk',
        url: advantagesNetwork,
        alt: 'Fagligt netværk',
        previewClass: photo,
        aspectRatio: '500 / 333',
      },
      {
        id: 'events',
        label: 'Arrangementer',
        url: events,
        alt: 'Arrangement for medlemmer',
        previewClass: photo,
        aspectRatio: '500 / 333',
      },
      {
        id: 'mentoring',
        label: 'Mentorordning',
        url: mentoring,
        alt: 'Mentorordning',
        previewClass: card,
      },
      {
        id: 'sparring',
        label: 'Faglig sparring',
        url: sparring,
        alt: 'Faglig sparring',
        previewClass: card,
      },
      {
        id: 'magazine',
        label: 'Fagblad',
        url: magazine,
        alt: 'Faglig viden og inspiration',
        previewClass: card,
      },
    ],
  },
]

export function getSiteImageGroup(group: string) {
  return siteImageGroups.find((definition) => definition.id === group)
}

export function getSiteImageSlot(group: string, slot: string) {
  const definition = getSiteImageGroup(group)?.slots.find((definition) => definition.id === slot)
  if (!definition) throw new Error('Billedfeltet findes ikke.')
  return definition
}

export const maxSiteImageBytes = 5 * 1024 * 1024
export const siteImageAccept = 'image/jpeg,image/png,image/webp'

export function validateSiteImageFile(file: File): string | undefined {
  if (!file.size) return 'Billedfilen er tom.'
  if (file.size > maxSiteImageBytes) return 'Billedfilen må højst fylde 5 MB.'
  if (!siteImageAccept.split(',').includes(file.type))
    return 'Vælg et JPEG-, PNG- eller WebP-billede.'
}
