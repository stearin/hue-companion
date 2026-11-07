# Hue Companion website

The product website is a separate Next.js App Router project, outside `HueCompanion.sln`. It contains product, feature, privacy, and MCP setup pages.

## Development

Run from `website/` with Node.js and npm installed. Use a Node runtime compatible with the Next.js version declared in `package.json`.

```bash
npm ci
npm run dev
```

Open `http://localhost:3000`. Edit the home page at `src/app/page.tsx`; shared components live in `src/components/`, SEO configuration in `src/lib/seo.ts`, and static assets in `public/`.

`src/app/layout.tsx` loads Inter and Space Grotesk through `next/font/google`. Production builds need access to retrieve those fonts.

## Validation

```bash
npm run lint
npm run build
```

Run both scripts after website code changes. Dependencies and exact versions are recorded in `package.json` and `package-lock.json`.

## Production output

`next.config.ts` selects `output: "standalone"`. `npm run build` produces the standalone server under `.next/standalone/`. A standalone deployment also needs `public/` and `.next/static/` copied into the corresponding locations beside that server. The repository's .NET workflows do not deploy this website or establish a hosting provider.

After building and copying the static assets described above, launch the standalone server:

```bash
node .next/standalone/server.js
```

The package also defines `npm run start` as `next start`; use the generated server entry point for the configured standalone deployment.

See the repository [development guide](../docs/DEVELOPMENT.md) for the desktop and release workflows.
