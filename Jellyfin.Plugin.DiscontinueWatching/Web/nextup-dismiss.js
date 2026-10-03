/* Adds a per-user dismiss control to Home > Next Up. */
(() => {
  'use strict';
  if (window.__dismissWatchingNextUpLoaded) return;
  window.__dismissWatchingNextUpLoaded = true;

  const buttonClass = 'nextup-dismiss-button';
  const style = document.createElement('style');
  style.textContent = `
    .verticalSection:has(a[href*="nextup"]) .card { position: relative; }
    .${buttonClass} { position:absolute; top:8px; right:8px; z-index:20; width:32px; height:32px; border:0; border-radius:50%; background:rgba(0,0,0,.72); color:#fff; cursor:pointer; opacity:0; transform:scale(.8); transition:opacity .15s,transform .15s,background .15s; }
    .${buttonClass}:hover, .${buttonClass}:focus { opacity:1; transform:scale(1); background:#c62828; }
  `;
  document.head.appendChild(style);

  const headers = () => ({ Authorization: `MediaBrowser Token="${ApiClient.accessToken()}"` });
  const api = path => `${ApiClient.serverAddress()}/DismissWatching${path}`;
  const nextUpSection = () => [...document.querySelectorAll('.verticalSection')]
    .find(section => section.querySelector('a[href*="nextup"]'));

  async function loadDenylist() {
    const response = await fetch(api('/Items'), { headers: headers() });
    if (!response.ok) throw new Error(`denylist request failed: ${response.status}`);
    return new Set(await response.json());
  }

  async function nextUpSeriesIds() {
    const userId = ApiClient.getCurrentUserId();
    const response = await fetch(`${ApiClient.serverAddress()}/Shows/NextUp?userId=${userId}&limit=100&enableTotalRecordCount=false`, { headers: headers() });
    if (!response.ok) throw new Error(`Next Up request failed: ${response.status}`);
    return new Map((await response.json()).Items?.map(item => [item.Id, item.SeriesId || item.Id]));
  }

  function hideSeries(seriesId) {
    const section = nextUpSection();
    if (!section) return;
    section.querySelectorAll('.card[data-id]').forEach(card => {
      if (card.dataset.dismissWatchingSeriesId === seriesId || card.dataset.id === seriesId) card.remove();
    });
    if (!section.querySelector('.card')) section.style.display = 'none';
  }

  async function apply() {
    if (!window.ApiClient?.accessToken?.()) return;
    let denylist, seriesIds;
    try { [denylist, seriesIds] = await Promise.all([loadDenylist(), nextUpSeriesIds()]); } catch (error) { console.error('[Dismiss Watching] Could not load Next Up data', error); return; }
    const section = nextUpSection();
    if (!section) return;
    section.querySelectorAll('.card[data-id]').forEach(card => {
      if (card.querySelector(`.${buttonClass}`)) return;
      const seriesId = seriesIds.get(card.dataset.id) || card.dataset.id;
      card.dataset.dismissWatchingSeriesId = seriesId;
      if (denylist.has(seriesId)) { card.remove(); return; }
      const button = document.createElement('button');
      button.className = buttonClass;
      button.type = 'button';
      button.title = 'Dismiss show from Next Up';
      button.textContent = '×';
      button.addEventListener('click', async event => {
        event.preventDefault(); event.stopPropagation();
        button.disabled = true;
        try {
          await fetch(api(`/Items/${seriesId}`), { method: 'POST', headers: headers() });
          hideSeries(seriesId);
        } catch (error) { console.error('[Dismiss Watching] Could not dismiss Next Up show', error); button.disabled = false; }
      });
      card.appendChild(button);
    });
  }

  new MutationObserver(() => { void apply(); }).observe(document.body, { childList: true, subtree: true });
  const timer = setInterval(() => { if (window.ApiClient?.accessToken?.()) { clearInterval(timer); void apply(); } }, 1000);
})();
