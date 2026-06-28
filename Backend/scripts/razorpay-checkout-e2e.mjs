import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { chromium } from 'playwright';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const harnessPath = path.join(__dirname, 'razorpay-checkout-harness.html');
const screenshotDir = path.join(__dirname, 'razorpay-e2e-screenshots');

function serveHarness(port) {
  const html = fs.readFileSync(harnessPath);
  return new Promise((resolve) => {
    const server = http.createServer((_, res) => {
      res.writeHead(200, { 'Content-Type': 'text/html' });
      res.end(html);
    });
    server.listen(port, () => resolve(server));
  });
}

async function tryPayInFrame(frame, page) {
  const mobile = frame.locator('input[type="tel"], input[placeholder*="mobile" i], input[name*="contact" i]').first();
  if (await mobile.count()) {
    await mobile.fill('9999999999');
    const cont = frame.locator('button', { hasText: /continue/i }).first();
    if (await cont.count()) await cont.click({ timeout: 8000 });
    await page.waitForTimeout(3000);
  }

  const attempts = [
    async () => {
      const success = frame.locator('button', { hasText: 'Success' }).first();
      if (await success.count()) { await success.click({ timeout: 8000 }); return true; }
    },
    async () => {
      const cardTab = frame.getByText('Card', { exact: false }).first();
      if (await cardTab.count()) await cardTab.click({ timeout: 5000 });
      const number = frame.locator('input[name="card.number"], input[placeholder*="card" i]').first();
      if (await number.count()) {
        await number.fill('4111111111111111');
        await frame.locator('input[name="card.expiry"], input[placeholder*="MM" i]').first().fill('12 / 30');
        await frame.locator('input[name="card.cvv"], input[placeholder*="CVV" i]').first().fill('123');
        const pay = frame.locator('button', { hasText: /pay/i }).first();
        if (await pay.count()) { await pay.click({ timeout: 8000 }); return true; }
      }
    },
    async () => {
      const netbanking = frame.getByText('Netbanking', { exact: false }).first();
      if (await netbanking.count()) await netbanking.click({ timeout: 5000 });
      const hdfc = frame.getByText('HDFC', { exact: false }).first();
      if (await hdfc.count()) await hdfc.click({ timeout: 5000 });
      const pay = frame.locator('button', { hasText: /pay|success/i }).first();
      if (await pay.count()) { await pay.click({ timeout: 8000 }); return true; }
    }
  ];

  for (const attempt of attempts) {
    try {
      if (await attempt()) return true;
    } catch { /* try next */ }
  }
  return false;
}

export async function runCheckout({ keyId, orderId, amountPaise, planName }) {
  fs.mkdirSync(screenshotDir, { recursive: true });
  const port = 8765;
  const server = await serveHarness(port);
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  const errors = [];
  page.on('pageerror', (e) => errors.push(e.message));

  const q = new URLSearchParams({
    key: keyId,
    order_id: orderId,
    amount: String(amountPaise),
    currency: 'INR',
    name: 'FitZone Demo Gym',
    description: planName || 'Basic Plan'
  });
  await page.goto(`http://127.0.0.1:${port}/?${q.toString()}`);
  await page.screenshot({ path: path.join(screenshotDir, '02-checkout-page.png'), fullPage: true });
  await page.click('#pay');
  await page.waitForTimeout(6000);
  await page.screenshot({ path: path.join(screenshotDir, '03-razorpay-popup.png'), fullPage: true });

  let popupOpened = false;
  let paid = false;
  let paymentPayload = null;

  for (const frame of page.frames()) {
    if (!/razorpay|checkout/i.test(frame.url())) continue;
    popupOpened = true;
    try {
      const frameEl = await frame.frameElement();
      if (frameEl) await frameEl.screenshot({ path: path.join(screenshotDir, '03-razorpay-iframe.png') });
    } catch { /* optional */ }
    if (await tryPayInFrame(frame, page)) {
      paid = true;
      break;
    }
  }

  if (!paid) {
    for (const frame of page.frames()) {
      const mobile = frame.locator('input').first();
      if (await mobile.count()) {
        try {
          await mobile.fill('9999999999');
          const cont = frame.locator('button').filter({ hasText: /continue/i }).first();
          if (await cont.count()) await cont.click({ timeout: 5000 });
        } catch { /* ignore */ }
      }
    }
    await page.waitForTimeout(4000);
    for (const frame of page.frames()) {
      if (await tryPayInFrame(frame, page)) { paid = true; break; }
    }
  }

  if (paid) {
    await page.waitForFunction(() => {
      const el = document.getElementById('result');
      return el && el.textContent && el.textContent.includes('razorpay_payment_id');
    }, { timeout: 30000 });
    paymentPayload = JSON.parse(await page.locator('#result').textContent());
    await page.screenshot({ path: path.join(screenshotDir, '04-payment-success.png'), fullPage: true });
  }

  await browser.close();
  server.close();
  return {
    popupOpened,
    paid,
    paymentPayload,
    jsErrors: errors,
    screenshots: {
      checkoutPage: path.join(screenshotDir, '02-checkout-page.png'),
      razorpayPopup: path.join(screenshotDir, '03-razorpay-popup.png'),
      razorpayIframe: path.join(screenshotDir, '03-razorpay-iframe.png'),
      paymentSuccess: paid ? path.join(screenshotDir, '04-payment-success.png') : null
    }
  };
}

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  const [keyId, orderId, amountPaise, planName] = process.argv.slice(2);
  runCheckout({ keyId, orderId, amountPaise: Number(amountPaise), planName })
    .then((r) => { console.log(JSON.stringify(r, null, 2)); process.exit(r.paid ? 0 : 1); })
    .catch((e) => { console.error(e); process.exit(2); });
}
