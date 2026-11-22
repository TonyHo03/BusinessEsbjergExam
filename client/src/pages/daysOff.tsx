import {useEffect, useState} from "react";
import * as React from "react";
import {ChevronLeft, ChevronRight, Plus, Trash2} from "lucide-react";
import {api} from "@/api/client.ts";
import {type DayOffDto} from "@/api/Api.ts";
import {formatDate} from "@/lib/format.ts";
import {Label} from "@/components/ui/label.tsx";
import {Separator} from "@/components/ui/separator.tsx";
import {Input} from "@/components/ui/input.tsx";
import {Button} from "@/components/ui/button.tsx";
import {Badge} from "@/components/ui/badge.tsx";
import {Table, TableBody, TableCell, TableHead, TableHeader, TableRow} from "@/components/ui/table.tsx";
import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle
} from "@/components/ui/dialog.tsx";

const today = () => new Date().toLocaleDateString("sv-SE"); // YYYY-MM-DD i lokal tid
const WEEKDAYS = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

// Helligdage og firmafridage koster ingen feriedage. Kun admin kan ændre dem, og kun fra i dag og frem
export function DaysOffView() {
    const [year, setYear] = useState<number>(new Date().getFullYear());
    const [days, setDays] = useState<DayOffDto[]>([]);
    const [open, setOpen] = useState(false);
    const [confirmDelete, setConfirmDelete] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const load = () => api.api.daysOffGet({year}).then(setDays).catch(() => setError("Could not load the days off."));

    useEffect(() => { setError(null); load(); }, [year]);

    const handleDelete = async (id: string) => {
        setError(null);
        try {
            await api.api.daysOffDelete({id});
            await load();
        } catch (err: any) {
            const problem = await err?.json?.().catch(() => null);
            setError(problem?.detail ?? "Could not delete the day off.");
        } finally {
            setConfirmDelete(null);
        }
    };

    return (
        <div className="flex flex-col w-full h-full p-12 gap-4">
            <div className="flex w-full">
                <Label className="text-[#0F2338] text-3xl font-normal">Days off</Label>
                <button type="button" onClick={() => setOpen(true)}
                        className="flex h-9 items-center gap-2 rounded-lg bg-primary px-6 text-sm font-medium text-primary-foreground ml-auto">
                    <Plus className="size-4"/> Add day off
                </button>
            </div>
            <Label className="text-[#516080]">
                Public holidays and company days off never cost holiday. Only days from today can be added or deleted, so the past never changes.
            </Label>
            <Separator className="h-px"/>

            {error && (
                <p role="alert" className="rounded-lg border border-[#CE6B4C] bg-[#CE6B4C]/10 px-4 py-3 text-sm text-[#B5583B]">{error}</p>
            )}

            <div className="flex items-center gap-2">
                <Button variant="outline" size="icon" aria-label="Previous year" onClick={() => setYear(y => y - 1)}>
                    <ChevronLeft className="size-4"/>
                </Button>
                <span className="w-16 text-center text-lg font-medium text-[#023459]">{year}</span>
                <Button variant="outline" size="icon" aria-label="Next year" onClick={() => setYear(y => y + 1)}>
                    <ChevronRight className="size-4"/>
                </Button>
            </div>

            <div className="border rounded-xl bg-card">
                <Table>
                    <TableHeader className="bg-muted! rounded-t-xl overflow-hidden!">
                        <TableRow>
                            <TableHead>Date</TableHead>
                            <TableHead>Name</TableHead>
                            <TableHead>Type</TableHead>
                            <TableHead className="text-right"><span className="sr-only">Actions</span></TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody className="[&_td]:py-4 [&_td]:text-[#023459]">
                        {days.length === 0 && (
                            <TableRow>
                                <TableCell colSpan={4} className="text-center text-muted-foreground!">
                                    No days off in {year} yet. Add the public holidays so they are not counted as holiday.
                                </TableCell>
                            </TableRow>
                        )}
                        {days.map(d => {
                            const past = d.date! < today();
                            return (
                                <TableRow key={d.id}>
                                    <TableCell>
                                        <div className="flex flex-col">
                                            <span className="font-medium">{formatDate(d.date!)}</span>
                                            <span className="text-xs text-muted-foreground">{WEEKDAYS[new Date(d.date!).getDay()]}</span>
                                        </div>
                                    </TableCell>
                                    <TableCell>{d.name}</TableCell>
                                    <TableCell>
                                        <Badge className={d.isCompany ? "bg-[#CE6B4C]/15 text-[#B5583B]" : "bg-muted text-muted-foreground"}>
                                            {d.isCompany ? "Company day off" : "Public holiday"}
                                        </Badge>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        {!past && (confirmDelete === d.id ? (
                                            <div className="flex justify-end gap-1">
                                                <Button type="button" variant="destructive" size="sm" onClick={() => handleDelete(d.id!)}>Delete</Button>
                                                <Button type="button" variant="outline" size="sm" onClick={() => setConfirmDelete(null)}>Cancel</Button>
                                            </div>
                                        ) : (
                                            <Button type="button" variant="ghost" size="icon" aria-label={`Delete ${d.name}`}
                                                    onClick={() => setConfirmDelete(d.id!)}>
                                                <Trash2 className="size-4 text-destructive"/>
                                            </Button>
                                        ))}
                                    </TableCell>
                                </TableRow>
                            );
                        })}
                    </TableBody>
                </Table>
            </div>

            <AddDayOffDialog open={open} onClose={() => setOpen(false)} onAdded={added => { setYear(Number(added.slice(0, 4))); load(); }}/>
        </div>
    );
}

function AddDayOffDialog({open, onClose, onAdded}: { open: boolean, onClose: () => void, onAdded: (date: string) => void }) {
    const [date, setDate] = useState<string>(today());
    const [name, setName] = useState<string>("");
    const [isCompany, setIsCompany] = useState(false);
    const [errors, setErrors] = useState<string[]>([]);
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        if (!open) return;
        setDate(today());
        setName("");
        setIsCompany(false);
        setErrors([]);
    }, [open]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitting(true);
        setErrors([]);
        try {
            await api.api.daysOffAdd({date, name: name.trim(), isCompany});
            onAdded(date);
            onClose();
        } catch (err: any) {
            const body = await err?.json?.().catch(() => null);
            setErrors(Array.isArray(body) ? body : ["Could not add the day off."]);
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <Dialog open={open} onOpenChange={o => !o && onClose()}>
            <DialogContent className="sm:max-w-md border-2">
                <DialogHeader>
                    <DialogTitle>Add day off</DialogTitle>
                    <DialogDescription>Nobody spends holiday on this day. Requests already approved keep the hours they had.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit} className="grid gap-4">
                    <div className="grid gap-2">
                        <Label htmlFor="dayOffDate">Date</Label>
                        <Input id="dayOffDate" type="date" min={today()} value={date} onChange={e => setDate(e.target.value)} required/>
                    </div>
                    <div className="grid gap-2">
                        <Label htmlFor="dayOffName">Name</Label>
                        <Input id="dayOffName" maxLength={100} placeholder="Christmas Eve" value={name}
                               onChange={e => setName(e.target.value)} required/>
                    </div>
                    <label className="flex items-center gap-2 text-sm text-[#023459]">
                        <input type="checkbox" className="size-4 accent-[#CE6B4C]" checked={isCompany}
                               onChange={e => setIsCompany(e.target.checked)}/>
                        Company day off (not a public holiday)
                    </label>

                    {errors.length > 0 && (
                        <ul role="alert" className="text-sm text-destructive list-disc pl-5">
                            {errors.map(e => <li key={e}>{e}</li>)}
                        </ul>
                    )}

                    <DialogFooter>
                        <DialogClose render={<Button variant="outline" type="button">Cancel</Button>}/>
                        <Button type="submit" disabled={submitting}>Add</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}