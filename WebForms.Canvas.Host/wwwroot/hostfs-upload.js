window.hostFsUpload = {
  pickFiles: function (multiple, accept) {
    return new Promise(function (resolve) {
      const input = document.createElement('input');
      input.type = 'file';
      if (multiple) input.multiple = true;
      if (accept) input.accept = accept;
      input.style.display = 'none';
      document.body.appendChild(input);

      input.addEventListener('change', function () {
        const files = Array.from(input.files || []);
        document.body.removeChild(input);
        resolve(files);
      }, { once: true });

      input.click();
    });
  },

  uploadFiles: async function (multiple, accept) {
    const files = await window.hostFsUpload.pickFiles(multiple, accept);
    const form = new FormData();
    for (const f of files) {
      form.append('files', f, f.name);
    }

    const resp = await fetch('/api/hostfs/upload', {
      method: 'POST',
      body: form
    });

    if (!resp.ok) {
      const text = await resp.text();
      throw new Error(text || ('Upload failed with status ' + resp.status));
    }

    return await resp.json();
  }
};
