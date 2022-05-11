import Head from "next/head";
import styles from "./layout.module.css";
import { ReactNode } from "react";

export const siteTitle = "CONTOSO: File Handler";

export interface ILayoutProps {
  children: ReactNode;
  home?: boolean;
}

/**
 * Main page layout container
 * 
 * @param props 
 */
export default function Layout(props: ILayoutProps) {

  const { children } = props;

  return (
    <div className={styles.container}>
      <Head>
        <link rel="SHORTCUT ICON" href="/favicon.ico" type="image/x-icon" />
        <meta name="description" content="Enhanced file experience for Microsoft365" />
        <meta name="og:title" content={siteTitle} />
        <meta name="twitter:card" content="summary_large_image" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      </Head>
      <header className={styles.header}></header>
      <main>{children}</main>
    </div>
  );
}
