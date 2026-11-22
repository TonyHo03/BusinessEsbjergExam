import {useEffect, useState} from "react";
import * as React from "react";
import {api} from "@/api/client.ts";
import {Label} from "@/components/ui/label.tsx";
import {Input} from "@/components/ui/input.tsx";
import {Button} from "@/components/ui/button.tsx";
import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle
} from "@/components/ui/dialog.tsx";

// Medarbejderen anmoder om at overføre uge 1–5-dage, der udløber 31.12, til næste ferieår.
// Bogholderiet godkender eller afviser. Reglerne tjekkes på serveren.
export function TransferDialog({employeeId, earningYear, remainingDays, onClose, onSent}: {
    employeeId: string,
    earningYear: number | null,
    remainingDays: number,
    onClose: () => void,
    onSent: () => void
}) {
    const [days, setDays] = useState<string>("");
    const [comment, setComment] = useState<string>("");
    const [errors, setErrors] = useState<string[]>([]);
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        if (earningYear === null) return;
        setDays(String(Math.min(5, Math.floor(remainingDays))));
        setComment("");
        setErrors([]);
    }, [earningYear]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (earningYear === null) return;
        setSubmitting(true);
        setErrors([]);
        try {
            await api.transfers.transfersCreate({
                employeeId,
                fromEarningYear: earningYear,
                days: Number(days),
                comment: comment.trim() || null,
            });
            onSent();
            onClose();
        } catch (err: any) {
            const body = await err?.json?.().catch(() => null);
            setErrors(Array.isArray(body) ? body : ["Could not send the transfer request."]);
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <Dialog open={earningYear !== null} onOpenChange={open => !open && onClose()}>
            <DialogContent className="sm:max-w-md border-2">
                <DialogHeader>
                    <DialogTitle>Request a transfer</DialogTitle>
                    <DialogDescription>
                        Holiday above 4 weeks (at most 5 days) can be moved to the next holiday year with a written agreement.
                        Accounting approves or rejects the request.
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit} className="grid gap-4">
                    <div className="grid gap-2">
                        <Label htmlFor="transferDays">Days to transfer</Label>
                        <Input id="transferDays" type="number" min={0.5} max={5} step={0.5} value={days}
                               onChange={e => setDays(e.target.value)} required/>
                        <span className="text-xs text-muted-foreground">
                            {remainingDays.toLocaleString("da-DK")} days left from the period earned in {earningYear}
                        </span>
                    </div>
                    <div className="grid gap-2">
                        <Label htmlFor="transferComment">Comment (optional)</Label>
                        <Input id="transferComment" maxLength={500} value={comment} onChange={e => setComment(e.target.value)}/>
                    </div>

                    {errors.length > 0 && (
                        <ul role="alert" className="text-sm text-destructive list-disc pl-5">
                            {errors.map(e => <li key={e}>{e}</li>)}
                        </ul>
                    )}

                    <DialogFooter>
                        <DialogClose render={<Button variant="outline" type="button">Cancel</Button>}/>
                        <Button type="submit" disabled={submitting}>Send request</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}