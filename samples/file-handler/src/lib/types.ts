import { NextApiRequest } from "next";
import { Session } from "next-iron-session";

export interface IActivationProps {
    appId: string;
    client: string;
    cultureName: string;
    domainHint: string;
    /**
     * array of urls
     */
    items: string[];
    userId: string;
    content: string;
}

export interface NextApiRequestWithSession extends NextApiRequest {
    session: Session;
}
