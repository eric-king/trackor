export async function generateDownloadUrl() {

    window.sqlitedb = window.sqlitedb || {
        init: false,
        cache: await caches.open('SqliteWasmHelper')
    };

    const resp = await window.sqlitedb.cache.match('/data/cache/trackor.db');

    if (resp && resp.ok) {
        const dbBlob = await resp.blob();
        if (dbBlob) {
            return URL.createObjectURL(dbBlob);
        }
    }

    return '';
}
