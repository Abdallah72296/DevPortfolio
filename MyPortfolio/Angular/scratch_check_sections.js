const fs = require('fs');

const mediaHtml = fs.readFileSync('C:/Users/Abd-Allah Abd-ElTawa/.gemini/antigravity-ide/brain/df3f14e0-1e15-4ed2-8675-f079a333fff0/.user_uploaded/media_1791561958167.html', 'utf8');
const currentHtml = fs.readFileSync('src/app/pages/public-portfolio-desktop/public-portfolio-desktop.component.html', 'utf8');

// Extract body from mediaHtml:
const mediaBodyStart = mediaHtml.indexOf('<body');
const mediaBodyEnd = mediaHtml.indexOf('</body>');
const mediaBody = mediaHtml.substring(mediaHtml.indexOf('>', mediaBodyStart) + 1, mediaBodyEnd).trim();

console.log('Media body length:', mediaBody.length);
console.log('Current HTML length:', currentHtml.length);

// Let's check sections in mediaBody:
const mediaSections = [];
const regex = /<(section|header|footer|div\s+class="[^"]*telemetry[^"]*")[^>]*>/gi;
let match;
while ((match = regex.exec(mediaBody)) !== null) {
  mediaSections.push(match[0].substring(0, 50));
}
console.log('Media sections found:', mediaSections.length);

// Check if any headings or text in mediaBody are missing from currentHtml:
const headings = [
  'Architecting ultra-low latency',
  'FEATURED_PROJECTS',
  'CORE_SKILLS',
  'CAREER_TRACK',
  'CREDENTIALS',
  'Featured Engineering Systems',
  'HyperScale KV Store',
  'CloudNative Gateway',
  'Neural Vector Index',
  'FinTech Ledger Core',
  'Verified Engineering Competency',
  'Languages & Runtimes',
  'Architecture & Cloud',
  'Frontend & UI Systems',
  'Systems & Observability',
  'Verified Engineering Leadership',
  'Stripe',
  'Datadog',
  'Cloudflare',
  'Formal Foundations & Certifications',
  'UC Berkeley',
  'CKA Administrator',
  'AWS Solutions Architect',
  'Initiate an Engineering Consultation',
  'Let’s talk architecture',
  'Your Name',
  'Organization / Firm',
  'Work Email',
  'Inbound Purpose',
  'Project Scope / Technical Problem',
  'Send Encrypted Message',
  'All Systems Operational'
];

headings.forEach(h => {
  const inMedia = mediaBody.includes(h);
  const inCurrent = currentHtml.includes(h);
  if (!inCurrent) {
    console.log('MISSING in current HTML:', h);
  }
});
console.log('Headings check done.');
