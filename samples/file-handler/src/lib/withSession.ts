import { Handler, withIronSession } from "next-iron-session";

export function withSession(handler: Handler) {

    const isProd = process.env.NODE_ENV === "production";
    const password = isProd ? process.env.IRON_SESSION_PASSWORD : "a1a2fec0-252b-499c-af1c-ce4bef7f2351";

    return withIronSession(handler, {
        cookieName: "msal-service-auth",
        cookieOptions: {
            sameSite: "strict",
            secure: isProd,
        },
        password,
    });
}
