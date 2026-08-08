import { Component, ElementRef, ViewChild, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import * as L from 'leaflet';
import { DeliveryLocationService } from '../../services/delivery-location.service';

// Default map center: DFC — Dream Mall, Wah Cantt
const DEFAULT_CENTER: L.LatLngTuple = [33.7473, 72.7456];

const PIN_ICON = L.divIcon({
  className: 'dfc-map-pin',
  html: `<svg width="34" height="44" viewBox="0 0 34 44" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path d="M17 0C7.6 0 0 7.6 0 17c0 12 17 27 17 27s17-15 17-27C34 7.6 26.4 0 17 0Z" fill="#f59e0b"/>
    <circle cx="17" cy="17" r="7" fill="#0f172a"/>
  </svg>`,
  iconSize: [34, 44],
  iconAnchor: [17, 44]
});

@Component({
  selector: 'app-location-picker-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './location-picker-modal.component.html',
  styleUrl: './location-picker-modal.component.css'
})
export class LocationPickerModalComponent {
  @ViewChild('mapEl') mapEl?: ElementRef<HTMLDivElement>;

  address = signal<string>('');
  locating = signal(false);
  resolvingAddress = signal(false);

  private map: L.Map | null = null;
  private marker: L.Marker | null = null;
  private selected: L.LatLngTuple = DEFAULT_CENTER;

  constructor(public deliveryLocation: DeliveryLocationService) {
    effect(() => {
      if (this.deliveryLocation.isOpen()) {
        setTimeout(() => this.initMap(), 0);
      } else {
        this.destroyMap();
      }
    });
  }

  private initMap() {
    if (!this.mapEl || this.map) return;

    const saved = this.deliveryLocation.location();
    this.selected = saved ? [saved.lat, saved.lng] : DEFAULT_CENTER;
    this.address.set(saved?.address ?? '');

    this.map = L.map(this.mapEl.nativeElement).setView(this.selected, 15);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
      maxZoom: 19
    }).addTo(this.map);

    this.marker = L.marker(this.selected, { icon: PIN_ICON, draggable: true }).addTo(this.map);

    this.marker.on('dragend', () => {
      const pos = this.marker!.getLatLng();
      this.selected = [pos.lat, pos.lng];
      this.reverseGeocode();
    });

    this.map.on('click', (e: L.LeafletMouseEvent) => {
      this.selected = [e.latlng.lat, e.latlng.lng];
      this.marker!.setLatLng(this.selected);
      this.reverseGeocode();
    });

    if (!saved) {
      this.reverseGeocode();
    }
  }

  private destroyMap() {
    this.map?.remove();
    this.map = null;
    this.marker = null;
  }

  useCurrentLocation() {
    if (!navigator.geolocation) {
      alert('Geolocation is not supported by your browser.');
      return;
    }

    this.locating.set(true);
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        this.selected = [pos.coords.latitude, pos.coords.longitude];
        this.map?.setView(this.selected, 16);
        this.marker?.setLatLng(this.selected);
        this.locating.set(false);
        this.reverseGeocode();
      },
      () => {
        this.locating.set(false);
        alert("Couldn't get your current location. Please allow location access, or pick a point on the map.");
      },
      { enableHighAccuracy: true, timeout: 10000 }
    );
  }

  private reverseGeocode() {
    this.resolvingAddress.set(true);
    const [lat, lng] = this.selected;
    fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&zoom=18`)
      .then(res => res.json())
      .then(data => {
        this.address.set(data?.display_name ?? `${lat.toFixed(5)}, ${lng.toFixed(5)}`);
        this.resolvingAddress.set(false);
      })
      .catch(() => {
        this.address.set(`${lat.toFixed(5)}, ${lng.toFixed(5)}`);
        this.resolvingAddress.set(false);
      });
  }

  confirm() {
    const [lat, lng] = this.selected;
    this.deliveryLocation.setLocation({
      lat,
      lng,
      address: this.address() || `${lat.toFixed(5)}, ${lng.toFixed(5)}`
    });
  }

  close() {
    this.deliveryLocation.close();
  }
}
