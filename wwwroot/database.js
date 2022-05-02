const cacheName = 'SqliteWasmHelper';
const trackorDbPath = '/data/cache/trackor.db';

export async function generateDownloadUrl() {

    window.sqlitedb = window.sqlitedb || {
        init: false,
        cache: await caches.open(cacheName)
    };

    const match = await window.sqlitedb.cache.match(trackorDbPath);

    if (match && match.ok) {
        const dbBlob = await match.blob();
        if (dbBlob) {
            return URL.createObjectURL(dbBlob);
        }
    }

    return '';
}

export async function deleteDatabase() {

    window.sqlitedb = window.sqlitedb || {
        init: false,
        cache: await caches.open(cacheName)
    };

    const match = await window.sqlitedb.cache.match(trackorDbPath);

    if (match && match.ok) {

        const deleteResp = await window.sqlitedb.cache.delete(trackorDbPath);

        if (deleteResp) {
            return true;
        }
    }

    return false;
}

export async function uploadDatabase(blob) {

    window.sqlitedb = window.sqlitedb || {
        init: false,
        cache: await caches.open(cacheName)
    };

    const match = await window.sqlitedb.cache.match(trackorDbPath);

    if (match && match.ok) {

        await window.sqlitedb.cache.delete(trackorDbPath);
    }

    const headers = new Headers({
        'content-length': blob.size
    });

    const response = new Response(blob, {
        headers
    });

    await window.sqlitedb.cache.put(trackorDbPath, response);
}
