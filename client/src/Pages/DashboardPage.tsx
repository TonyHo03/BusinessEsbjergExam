import { useNavigate } from "react-router";
import { Plus } from "lucide-react";
import { Button } from "../components/Button";
import { PageHeader } from "../components/PageHeader";

export function DashboardPage() {
    const navigate = useNavigate();

    return (
        <PageHeader
            title="A little time to look forward to"
            description="Here’s where your holiday balance stands. Coming in #24."
            actions={
                <Button onClick={() => navigate("/requests/new")}>
                    <Plus className="size-4" aria-hidden />
                    Request holiday
                </Button>
            }
        />
    );
}