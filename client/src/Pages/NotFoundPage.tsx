import { Link } from "react-router";
import { PageHeader } from "../components/PageHeader";

export function NotFoundPage() {
    return (
        <>
            <PageHeader title="Page not found" description="The page you are looking for does not exist." />
            <Link to="/">Back to dashboard</Link>
        </>
    );
}