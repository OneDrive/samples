import Head from "next/head";
import Layout, { siteTitle } from "../components/layout";

/**
 * Basic index page to allow us to see if the server is running without having to craft any requests
 */
const Home = () => {
  return (
    <Layout home>
      <Head>
        <title>{siteTitle}</title>
      </Head>
      <section>
        <p>Hello, server is running 😀!</p>
      </section>
    </Layout>
  );
};
export default Home;
