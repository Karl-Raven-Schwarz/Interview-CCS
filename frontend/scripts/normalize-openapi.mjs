// Recorta el contrato OpenAPI a un solo media type (application/json).
// ASP.NET publica variantes redundantes (text/json, text/plain) que hacen que el
// cliente generado duplique métodos con sufijos ($Json / $Plain) sin aportar nada.
import { readFileSync, writeFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const file = resolve(process.argv[2] ?? resolve(here, '../../docs/openapi.json'));

const doc = JSON.parse(readFileSync(file, 'utf8'));

for (const pathItem of Object.values(doc.paths ?? {})) {
  for (const operation of Object.values(pathItem)) {
    if (!operation || typeof operation !== 'object' || !operation.responses) {
      continue;
    }

    const request = operation.requestBody?.content;
    if (request?.['application/json']) {
      operation.requestBody.content = { 'application/json': request['application/json'] };
    }

    for (const response of Object.values(operation.responses)) {
      const content = response.content;
      if (content?.['application/json']) {
        response.content = { 'application/json': content['application/json'] };
      }
    }
  }
}

writeFileSync(file, `${JSON.stringify(doc, null, 2)}\n`);
console.log(`Contrato normalizado a application/json: ${file}`);
