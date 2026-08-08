import { Component, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SiteSettingService } from '../../services/site-setting.service';

@Component({
  selector: 'app-whatsapp-float',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './whatsapp-float.component.html',
  styleUrl: './whatsapp-float.component.css'
})
export class WhatsappFloatComponent {
  waLink = computed(() => {
    const raw = this.siteSetting.whatsAppNumber();
    if (!raw) return null;
    const digits = raw.replace(/\D/g, '');
    return digits ? `https://wa.me/${digits}` : null;
  });

  constructor(public siteSetting: SiteSettingService) {}
}
